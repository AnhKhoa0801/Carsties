"use client";
import { placedBidForAuction } from "@/app/actions/auctionActions";
import { numberWithCommas } from "@/app/lib/numberWithComma";
import { useBidStore } from "@/hooks/useBidStore";
import React from "react";
import { FieldValues, useForm } from "react-hook-form";
import toast from "react-hot-toast";
type Props = {
  autctionId: string;
  highBid: number;
};

export default function BidForm({ autctionId, highBid }: Props) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm();

  const addBid = useBidStore((state) => state.addBid);

  function onSubmit(data: FieldValues) {
    placedBidForAuction(autctionId, data.amount)
      .then((bid) => {
        if (bid.error) throw bid.error;
        addBid(bid);
        reset();
      })
      .catch((error) => toast.error(error.message));
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex items-center order-2 rounded-lg py-2"
    >
      <input
        type="number"
        {...register("amount")}
        className="input-custom text-sm text-gray-600"
        placeholder={`enter your bid (minimum is $${numberWithCommas(
          highBid + 1
        )})`}
      />
    </form>
  );
}
