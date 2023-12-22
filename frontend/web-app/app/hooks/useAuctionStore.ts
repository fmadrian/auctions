import { create } from "zustand";
import { Auction, PagedResult } from "../types/AppTypes";
import { createWithEqualityFn } from "zustand/traditional";

// Store for auctions.
type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};
type Actions = {
  setData: (data: PagedResult<Auction>) => void;
  setCurrentPrice: (auctionId: string, amount: number) => void;
};

const initialState: State = {
  auctions: [],
  totalCount: 0,
  pageCount: 0,
};

// Create hook for state.
export const useAuctionStore = createWithEqualityFn<State & Actions>()(
  (set) => ({
    ...initialState,
    // Implement actions (methods) inside the hook.
    setData: (data: PagedResult<Auction>) => {
      set(() => ({
        auctions: data.results,
        totalCount: data.results.length,
        pageCount: data.pageCount,
      }));
    },
    setCurrentPrice: (auctionId: string, amount: number) => {
      set((state) => ({
        ...state,
        auctions: state.auctions.map(
          (auction) =>
            auction.id === auctionId // On the auctions where the ID matches
              ? {
                  ...auction,
                  currentHighBid: auction.currentHighBid,
                } // Return the auction with a different currentHighBid.
              : auction // Return the auction on all the other ones.
        ),
      }));
    },
  })
);
