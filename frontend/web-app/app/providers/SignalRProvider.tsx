"use client";
import { useAuctionStore } from "@/hooks/useAuctionStore";
import { useBidStore } from "@/hooks/useBidStore";
import { Auction, AuctionFinished, Bid } from "@/types";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { User } from "next-auth";
import React, { ReactNode, useEffect } from "react";
import toast from "react-hot-toast";
import AuctionCreatedToast from "../components/AuctionCreatedToast";
import AuctionFinishedToast from "../components/AuctionFinishedToast";
import { getDetailsViewData } from "../actions/auctionActions";

type Props = {
  children: ReactNode;
  user: User | null;
};

export default function SignalRProvider({ children, user }: Props) {
  const [connection, setConnection] = React.useState<HubConnection | null>(
    null
  );
  const setCurretnPrice = useAuctionStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(process.env.NEXT_PUBLIC_NOTIFY_URL!)
      .withAutomaticReconnect()
      .build();
    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log("connection to notification hub");

          connection.on("BidPlaced", (bid: Bid) => {
            console.log("BidPlaced", bid);
            if (bid.bidStatus.includes("Accepted")) {
              setCurretnPrice(bid.auctionId, bid.amount);
            }
            addBid(bid);
          });

          connection.on("AuctionCreated", (auction: Auction) => {
            if (user?.username !== auction.seller) {
              return toast(<AuctionCreatedToast auction={auction} />, {
                duration: 10000,
              });
            }
          });

          connection.on(
            "AuctionsFinished",
            (finishedAuction: AuctionFinished) => {
              const auction = getDetailsViewData(finishedAuction.auctionId);
              return toast.promise(
                auction,
                {
                  loading: "loading",
                  success: (auction) => (
                    <AuctionFinishedToast
                      finishedAuction={finishedAuction}
                      auction={auction}
                    />
                  ),
                  error: (err) => "auction finished!",
                },
                { success: { duration: 10000, icon: null } }
              );
            }
          );
        })
        .catch((error) => console.log("Error with signalR", error));
    }

    return () => {
      connection?.stop();
    };
  }, [connection, setCurretnPrice, addBid, user]);

  return children;
}
