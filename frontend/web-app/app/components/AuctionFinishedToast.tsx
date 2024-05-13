import { Auction, AuctionFinished } from "@/types";
import Image from "next/image";
import Link from "next/link";
import React from "react";
import { numberWithCommas } from "../lib/numberWithComma";
type Props = {
  finishedAuction: AuctionFinished;
  auction: Auction;
};

export default function AuctionFinishedToast({
  finishedAuction,
  auction,
}: Props) {
  return (
    <Link
      href={`/auctions/details/${auction.id}`}
      className="flex flex-col items-center"
    >
      <div className=" flex flex-col items-center gap-2">
        <Image
          src={auction.imageUrl}
          alt="image"
          height={80}
          width={80}
          className="rounded-lg w-auto h-auto"
        />

        <div className="flex flex-col">
          <span>
            auction for {auction.make} {auction.model} has finished
            {finishedAuction.itemSold && finishedAuction.amount ? (
              <p>
                congrats to {finishedAuction.winner} who has won this auction
                for ${numberWithCommas(finishedAuction.amount)}{" "}
              </p>
            ) : (
              <p>this item did not sell</p>
            )}
          </span>
        </div>
      </div>
    </Link>
  );
}
