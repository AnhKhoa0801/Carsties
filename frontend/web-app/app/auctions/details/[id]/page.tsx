import { getDetailsViewData } from "@/app/actions/auctionActions";
import Heading from "@/app/components/Heading";
import CountdownTimer from "../../CountdownTimer";

export default async function Details({ params }: { params: { id: string } }) {
  const data = await getDetailsViewData(params.id);
  return (
    <div>
      <div className="flex justify-between">
        <Heading title={`${data.make} ${data.model}`} />
        <div className="flex gap-3">
          <h3 className="text-2xl font-semibold">Time remaining:</h3>
          <CountdownTimer auctionEnd={data.auctionEnd} />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-6 mt-3"></div>
    </div>
  );
}
