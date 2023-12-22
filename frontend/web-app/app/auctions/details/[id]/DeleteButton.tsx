"use client";
import { deleteAuction } from "@/app/actions/auctionActions";
import { Button } from "flowbite-react";
import { useRouter } from "next/navigation";
import React, { useState } from "react";
import toast from "react-hot-toast";
type Props = {
  id: string;
};
export default function DeleteButton({ id }: Props) {
  const [isLoading, setIsLoading] = useState(false);
  // const [username, setUsername] = useState("");
  const router = useRouter();

  /*useEffect(() => {
    getCurrentUser().then((data) => {
      if (data) setUsername(data.username);
    });
  });*/
  function doDelete() {
    setIsLoading(true);
    deleteAuction(id)
      .then((response) => {
        if (response.error) {
          throw response.error;
        }
        // router.push(`/?seller=${username}`);
        router.push(`/`);
      })
      .catch((error) => toast.error(error.message))
      .finally(() => setIsLoading(false));
  }
  return (
    <Button color="failure" isProcessing={isLoading} onClick={doDelete}>
      Delete auction
    </Button>
  );
}
