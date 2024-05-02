"use server";
import { Auction, Bid, PagedResult } from "@/types";
import { fetchWrapper } from "@/lib/fetchWrapper";
import { FieldValues } from "react-hook-form";
import { revalidatePath } from "next/cache";
export default async function getData(
  query: string
): Promise<PagedResult<Auction>> {
  return await fetchWrapper.get(`search/${query}`);
}

export async function updateAuctionTest() {
  const data = {
    mileage: Math.floor(Math.random() * 100000),
  };

  return await fetchWrapper.put(
    `auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c`,
    data
  );
}

export async function createAuction(data: FieldValues) {
  return await fetchWrapper.post(`auctions`, data);
}

export async function getDetailsViewData(id: string): Promise<Auction> {
  return await fetchWrapper.get(`auctions/${id}`);
}

export async function updateAuction(id: string, data: FieldValues) {
  const res = await fetchWrapper.put(`auctions/${id}`, data);
  revalidatePath(`/auctions/${id}`);
  return res;
}

export async function deleteAuction(id: string) {
  return await fetchWrapper.del(`auctions/${id}`);
}

export async function getBidsForAuction(id: string): Promise<Bid[]> {
  return await fetchWrapper.get(`bids/${id}`);
}
