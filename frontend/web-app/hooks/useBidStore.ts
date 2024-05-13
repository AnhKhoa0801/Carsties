import { Bid } from "@/types";
import { createWithEqualityFn } from "zustand/traditional";

type State = {
  bids: Bid[];
};

type Action = {
  setBids: (bids: Bid[]) => void;
  addBid: (bid: Bid) => void;
};

export const useBidStore = createWithEqualityFn<State & Action>((set) => ({
  bids: [],
  setBids: (bids: Bid[]) => {
    set(() => ({
      bids: bids,
    }));
  },
  addBid: (bid: Bid) => {
    set((state) => ({
      bids: !state.bids.find((b) => b.id === bid.id)
        ? [...state.bids, bid]
        : [...state.bids],
    }));
  },
}));
