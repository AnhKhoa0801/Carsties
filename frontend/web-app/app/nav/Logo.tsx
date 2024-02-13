"use client";
import { useParamStore } from "@/hooks/useParamStore";
import { usePathname, useRouter } from "next/navigation";
import React from "react";
import { AiOutlineCar } from "react-icons/ai";

export default function Logo() {
  const router = useRouter();
  const pathName = usePathname();
  const reset = useParamStore((state) => state.reset);
  function doReset() {
    if (pathName !== "/") router.push("/");
    reset();
  }

  return (
    <div
      onClick={doReset}
      className="flex items-center gap-2 text-3xl font-semibold text-red-500 hover:cursor-pointer"
    >
      <AiOutlineCar size={34} />
      <div>Carsties Auctions</div>
    </div>
  );
}
