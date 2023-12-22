"use client";
import { placeBidForAuction } from "@/app/actions/auctionActions";
import { useBidStore } from "@/app/hooks/useBidStore";
import { numberWithCommas } from "@/app/lib/numberWithComma";
import React from "react";
import { FieldValues, useForm } from "react-hook-form";
import toast from "react-hot-toast";

type Props = {
  auctionId: string;
  highBid: number; // Use it to indicate what the minimum bid should be.
};

export default function BidForm({ auctionId, highBid }: Props) {
  // 1. Bring the following from React Hook Form.
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm();
  // 2. Get addBid from bid store.
  const addBid = useBidStore((state) => state.addBid);

  // 3. Define what to do on submit.
  function onSubmit(data: FieldValues) {
    // Don't submit bids that are not higher than the highest bid.
    if (data.amount <= highBid) {
      reset();
      return toast.error(
        `Bid must be at least $${numberWithCommas(highBid + 1)}`
      );
    }
    placeBidForAuction(auctionId, data.amount)
      .then((bid) => {
        if (bid.error) {
          throw bid.error;
        }
        // Reset the form.
        // Bid is added by SignalR client receiving a 'bid placed' event from the notifications service.
        reset();
      })
      .catch((error) => toast.error(error.message));
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex items-center border-2 rounded-lg py-2"
    >
      <input
        type="number"
        {...register("amount")}
        className="input-custom text-sm text-gray-600"
        placeholder={`Enter your bid (minimum bid is $${numberWithCommas(
          highBid
        )})`}
      />
    </form>
  );
}
