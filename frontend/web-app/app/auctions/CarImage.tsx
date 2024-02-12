"use client";
import Image from "next/image";
import React from "react";

type Props = {
  imageUrl: string;
};

export default function CarImage({ imageUrl }: Props) {
  const [isLoading, setIsLoading] = React.useState(true);
  return (
    <Image
      src={imageUrl}
      alt="image"
      fill
      priority
      className={`
	  	object-cover
	  	group-hover:opacity-75 
	  	duration-700
	  	ease-in-out
		${isLoading ? "grayscale blur-2xl scale-110" : "grayscale-0 blur-0 scale-100"}
	  `}
      sizes="(max-width:780px) 100vw, (max-width:1024px) 50vw, 33vw"
      onLoad={() => setIsLoading(false)}
    />
  );
}
