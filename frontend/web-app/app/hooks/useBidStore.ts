import { createWithEqualityFn } from "zustand/traditional";
import { Bid } from "../types/AppTypes";

type State = {
  bids: Bid[];
  open: boolean;
};

type Actions = {
  setBids: (bids: Bid[]) => void;
  addBid: (bid: Bid) => void;
  setOpen: (value: boolean) => void;
};
export const useBidStore = createWithEqualityFn<State & Actions>((set) => ({
  bids: [],
  open: true,

  setBids: (bids: Bid[]) => {
    set(() => ({
      bids,
    }));
  },
  addBid: (bid: Bid) => {
    set((state) => ({
      // If the new bid is not found inside the bids in the state, it adds it at the beginning of it.
      bids: !state.bids.find((b) => b.id === bid.id)
        ? [bid, ...state.bids]
        : [...state.bids],
    }));
  },
  setOpen: (value: boolean) => {
    set(() => ({
      open: value,
    }));
  },
}));
