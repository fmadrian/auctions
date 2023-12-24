"use client";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { ReactNode, useEffect, useState } from "react";
import { useAuctionStore } from "../hooks/useAuctionStore";
import { useBidStore } from "../hooks/useBidStore";
import { Auction, AuctionFinished, Bid } from "../types/AppTypes";
import { User } from "next-auth";
import AuctionCreatedToast from "../components/AuctionCreatedToast";
import toast from "react-hot-toast";
import AuctionFinishedToast from "../components/AuctionCreatedFinished";
import { getDetailedViewData } from "../actions/auctionActions";

type Props = {
  children: ReactNode;
  user: User | null;
};
// Parent (wrapper) component for the rest of the application.
export default function SignalRProvider({ children, user }: Props) {
  // SignalR Configuration
  // 1. Connection in component's state.
  const [connection, setConnection] = useState<HubConnection | null>(null);

  const setCurrentPrice = useAuctionStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);

  // ISSUE: Can't properly provide NEXT_PUBLIC_ env variables on different environments.
  // https://github.com/vercel/next.js/discussions/17641

  // WORKAROUND: Provide a hardcoded URL when we are in production. If we are not in production, use the .env variable
  const apiUrl =
    process.env.NODE_ENV === "production"
      ? "https://api.auctions.com/notifications"
      : process.env.NEXT_PUBLIC_NOTIFY_URL;

  // 2. Set the connection.
  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(apiUrl!)
      .withAutomaticReconnect() // Automatically reconnect if there is an issue.
      .build();

    setConnection(newConnection);
  }, [apiUrl]); // We need this to be called ONLY ONCE.
  // 3. Check and start the connection
  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log("Connected to notification hub.");
          // 4. Subscribe to event.
          connection.on(
            "BidPlaced", // It has to match the name of the event sent from the SignalR server.
            (bid: Bid) => {
              // Callback function that has a parameter object sent by the server on the message.
              // 5. Update only if a bid was accepted.
              if (bid.bidStatus.includes("Accepted")) {
                setCurrentPrice(bid.auctionId, bid.amount);
              }
              addBid(bid);
            }
          );

          // Listener for 'AuctionCreated' event.
          connection.on("AuctionCreated", (auction: Auction) => {
            // Show toast if the current user is not the one who created the auction
            if (user?.username !== auction.seller) {
              return toast(<AuctionCreatedToast auction={auction} />, {
                duration: 8000,
              });
            }
          });
          // Listener for 'AuctionFinished' event.
          connection.on(
            "AuctionFinished",
            async (finishedAuction: AuctionFinished) => {
              // How to pass promises inside toasts.
              const auction = getDetailedViewData(finishedAuction.auctionId);
              return toast.promise(
                auction,
                {
                  loading: "Loading",
                  success: (auction) => (
                    <AuctionFinishedToast
                      finishedAuction={finishedAuction}
                      auction={auction}
                    />
                  ),
                  error: "Auction finished",
                },
                { success: { duration: 10000, icon: null } }
              );
            }
          );
        })
        .catch((error) => console.error(error));
    }

    // When we dispose of this component, the code inside the return statement gets called.
    // REMEMBER: Destroy the connection when you dispose of the component.
    return () => {
      connection?.stop();
    };
  }, [connection, setCurrentPrice, addBid, user?.username]);

  return children;
}
