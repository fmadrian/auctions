import { numberWithCommas } from "@/app/lib/numberWithComma";
import { Bid } from "@/app/types/AppTypes";
import { format } from "date-fns";
import React from "react";
type Props = {
  bid: Bid;
};
export default function BidItem({ bid }: Props) {
  function getBidInfo() {
    let bgColor = "",
      text = "";
    switch (bid.bidStatus) {
      case "Accepted":
        bgColor = "bg-green-200";
        text = "Bid accepted";
        break;
      case "AcceptedBelowReserve":
        bgColor = "bg-amber-500";
        text = "Reserve not met";
        break;
      case "TooLow":
        bgColor = "bg-red-200";
        text = "Bid was too low";
        break;
      default:
        bgColor = "bg-red-200";
        text = "Bid placed after auction finished";
        break;
    }
    return { bgColor, text };
  }
  let { bgColor, text } = { ...getBidInfo() };
  return (
    <div
      className={`flex justify-between items-center mb-2 
    border-gray-300 border-2 px-3 py-2 rounded-lg ${bgColor} ${text}`}
    >
      <div className="flex flex-col">
        <span>Bidder: {bid.bidder}</span>
        <span className="text-gray-700 text-sm">
          Time:{" "}
          {
            // Use date-fnz to format, the Date object automatically converts the date to the user's local time.
            format(new Date(bid.bidTime), "dd MMM yyyy h:mm:ss a")
          }
        </span>
      </div>
      <div className="flex flex-col text-right">
        <div className="text-gray-700 text-sm">
          ${numberWithCommas(bid.amount)}
        </div>
        <div className="flex flex-row items-center">
          <span>{text}</span>
        </div>
      </div>
    </div>
  );
}
