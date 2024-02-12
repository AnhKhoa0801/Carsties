"use server";
import { Auction, PagedResult } from "@/types";
export default async function getData(
  url: string
): Promise<PagedResult<Auction>> {
  const res = await fetch(`http://localhost:6001/search${url}`);

  if (!res.ok) {
    throw new Error("Failed to fetch data");
  }

  return res.json();
}