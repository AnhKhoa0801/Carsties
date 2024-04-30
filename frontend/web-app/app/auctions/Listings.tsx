"use client";
import React, { useEffect, useState } from "react";
import AuctionCard from "./AuctionCard";
import AppPagination from "../components/AppPagination";
import getData from "../actions/auctionActions";
import Filters from "./Filters";
import { useParamStore } from "@/hooks/useParamStore";
import qs from "query-string";
import EmptyFilter from "../components/EmptyFilter";
import { useAuctionStore } from "@/hooks/useAuctionStore";

export default function Listings() {
  const [loading, setLoading] = useState(true);

  const params = useParamStore((state) => ({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    searchTerm: state.searchTerm,
    orderBy: state.orderBy,
    filterBy: state.filterBy,
    seller: state.seller,
    winner: state.winner,
  }));

  const data = useAuctionStore((state) => ({
    aucions: state.auctions,
    totalCount: state.totalCount,
    pageCount: state.pageCount,
  }));

  const setData = useAuctionStore((state) => state.setData);

  const setParams = useParamStore((state) => state.setParams);
  const url = qs.stringifyUrl({ url: "", query: params });

  const setPageNumber = (pageNumber: number) => {
    setParams({ pageNumber });
  };

  useEffect(() => {
    getData(url).then((data) => {
      setData(data);
      setLoading(false);
    });
  }, [url]);

  if (loading) return <h3>Loading...</h3>;

  if (data.totalCount === 0) return <EmptyFilter showReset />;

  return (
    <>
      <Filters />
      {data.totalCount === 0 ? (
        <EmptyFilter showReset />
      ) : (
        <>
          <div className="grid grid-cols-4 gap-6">
            {data.aucions.map((auction) => (
              <AuctionCard key={auction.id} auction={auction} />
            ))}
          </div>
          <div className="flex justify-center mt-4">
            <AppPagination
              currentPage={params.pageNumber}
              pageCount={data.pageCount}
              pageChanged={setPageNumber}
            />
          </div>
        </>
      )}
    </>
  );
}
