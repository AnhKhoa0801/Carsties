"use client";
import { useAuctionStore } from "@/hooks/useAuctionStore";
import { useBidStore } from "@/hooks/useBidStore";
import { Bid } from "@/types";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import React, { ReactNode, useEffect } from "react";

type Props = {
  children: ReactNode;
};

export default function SignalRProvider({ children }: Props) {
  const [connection, setConnection] = React.useState<HubConnection | null>(
    null
  );
  const setCurretnPrice = useAuctionStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl("http://localhost:6001/notifications")
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
        })
        .catch((error) => console.log("Error with signalR", error));
    }

    return () => {
      connection?.stop();
    };
  }, [connection, setCurretnPrice]);

  return children;
}
