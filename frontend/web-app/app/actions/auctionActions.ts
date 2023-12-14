"use server"; // Indicate this actions are executed on the server side.
import { FieldValues } from "react-hook-form";
import { fetchWrapper } from "../lib/fetchWrapper";
import { Auction, PagedResult } from "../types/AppTypes";
import { revalidatePath } from "next/cache";
// Actions folder could be equivalent to services.
// Guarantees that execution of code stays tied to client side.

export async function getData(query: string): Promise<PagedResult<Auction>> {
  return await fetchWrapper.get(`search${query}`);
}

export async function updateAuctionTest() {
  const data = {
    mileage: Math.floor(Math.random() * 100000) + 1,
  };
  return fetchWrapper.put(
    `auctions/afbee524-5972-4075-8800-7d11f9d7b0a0c`,
    data
  );
}

export async function createAuction(data: FieldValues) {
  return await fetchWrapper.post("auctions", data);
}
export async function getDetailedViewData(id: string): Promise<Auction> {
  return await fetchWrapper.get(`auctions/${id}`);
}
export async function updateAuction(id: string, data: FieldValues) {
  const response = await fetchWrapper.put(`auctions/${id}`, data);
  // Bypasses cache.
  revalidatePath(`/auctions/${id}`);
  return response;
}
export async function deleteAuction(id: string) {
  return await fetchWrapper.delet(`auctions/${id}`);
}
