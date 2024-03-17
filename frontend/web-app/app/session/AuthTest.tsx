"use client";
import React, { useState } from "react";
import { updateAuctionTest } from "../actions/auctionActions";
import { Button } from "flowbite-react";

export default function AuthTest() {
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>();

  function doUpdate() {
    setLoading(true);
    setResult(undefined);
    updateAuctionTest()
      .then((res) => {
        setResult(res);
      })
      .finally(() => {
        setLoading(false);
      });
  }
  return (
    <div className="flex items-center gap-4">
      <Button outline isProcessing={loading} onClick={doUpdate}>
        test auth
      </Button>
      <div>{JSON.stringify(result)}</div>
    </div>
  );
}
