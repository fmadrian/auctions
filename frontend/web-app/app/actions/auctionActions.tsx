"use server"; // Actions are executed on the server side.

import { Auction, PagedResult } from "../types/AppTypes";
// Actions folder could be equivalent to services.
// Guarantees that execution of code stays tied to client side.

export async function getData(query: string): Promise<PagedResult<Auction>> {
  const response = await fetch(`http://localhost:6001/search${query}`);
  if (!response.ok) throw new Error("Failed to fetch listings data.");

  return response.json();
}
