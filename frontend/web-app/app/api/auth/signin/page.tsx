import EmptyFilter from "@/app/components/EmptyFilter";

export default function Page({
  searchParams,
}: {
  searchParams: { callbackUrl: string };
}) {
  return <EmptyFilter showLogin callbackUrl={searchParams.callbackUrl} />;
}
