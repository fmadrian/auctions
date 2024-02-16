"use client";

import { getBidsForAuction } from "@/app/actions/auctionActions";
import Heading from "@/app/components/Heading";
import { useBidStore } from "@/app/hooks/useBidStore";
import { Auction, Bid } from "@/app/types/AppTypes";
import { User } from "next-auth";
import React, { useEffect, useState } from "react";
import toast from "react-hot-toast";
import BidItem from "./BidItem";
import EmptyFilter from "@/app/components/EmptyFilter";
import BidForm from "./BidForm";

type Props = {
  user: User | null;
  auction: Auction;
};
export default function BidList({ user, auction }: Props) {
  // 1. Set isLoading flag as part of the component's internal state.
  const [isLoading, setIsLoading] = useState(true);
  // 2. Retrieve bids for the auction passed as a prop.
  const bids = useBidStore((state) => state.bids);
  // 3. Retrieve set bids function from bid's store/state.
  const setBids = useBidStore((state) => state.setBids);
  // 4. Return the highest bid found on the bids in the state.
  const highBid = bids
    ? bids.reduce(
        (previousValue, currentValue) =>
          previousValue > currentValue.amount
            ? previousValue
            : currentValue.bidStatus.includes("Accepted")
            ? currentValue.amount
            : previousValue,
        0 // First/initial value
      )
    : 0;
  // 5. Return whether the auction opened is accepting bids or not.
  const open = useBidStore((state) => state.open);
  const setOpen = useBidStore((state) => state.setOpen);
  // 6. Indicate if the auction is still open for bids.
  const openForBids = new Date(auction.auctionEnd) > new Date();

  // 8. When we load the component for the first check if the auction is open for bids
  useEffect(() => {
    setOpen(openForBids);
  }, [openForBids, setOpen]);

  // 7. Indicate component to check for bids.
  useEffect(() => {
    getBidsForAuction(auction.id)
      .then((response: any) => {
        if (response.error) {
          throw response.error;
        }
        setBids(response as Bid[]); // 'as Bid[]' is a Cast.
      })
      .catch((err) => toast.error(err.message))
      .finally(() => {
        setIsLoading(false);
      });
  }, [auction.id, setIsLoading, setBids]); // Dependencies

  if (isLoading) {
    return (
      <div>
        <span>Loading...</span>
      </div>
    );
  } else {
    return (
      <div className="rounded-lg shadow-md">
        <div className="py-2 px-4 bg-white">
          <div className="sticky top-0 bg-white p-2">
            <Heading title={`Current high bid is $${highBid}`} />
          </div>
        </div>
        <div className="overflow-auto h-[400px] flex flex-col-reverse px-2">
          {bids.length === 0 ? (
            <EmptyFilter
              title="No bids for this item"
              subtitle="Please feel free to make a bid"
            />
          ) : (
            <>
              {bids.map((b) => (
                <BidItem key={b.id} bid={b} />
              ))}
            </>
          )}
        </div>
        {
          // Check if whether you the user is logged in or is allowed to bid on the auction.
        }
        <div className="px-2 pb-2 text-gray-500">
          {!open ? (
            <div className="flex items-center justify-center p-2 text-lg font-semibold">
              This auction has finished.
            </div>
          ) : !user ? (
            <div className="flex items-center justify-center p-2 text-lg font-semibold">
              Please login to make a bid.
            </div>
          ) : user && user.username === auction.seller ? (
            <div className="flex items-center justify-center p-2 text-lg font-semibold">
              {`You can't bid on your own auction`}
            </div>
          ) : (
            <BidForm auctionId={auction.id} highBid={highBid} />
          )}
        </div>
      </div>
    );
  }
}
