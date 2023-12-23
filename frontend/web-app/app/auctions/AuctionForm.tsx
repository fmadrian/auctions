"use client";
import { Button } from "flowbite-react";
import React, { useEffect } from "react";
import { FieldValues, useForm } from "react-hook-form";
import Input from "../components/Input";
import DateInput from "../components/DateInput";
import { createAuction, updateAuction } from "../actions/auctionActions";
import { usePathname, useRouter } from "next/navigation";
import toast from "react-hot-toast";
import { Auction } from "../types/AppTypes";

type Props = {
  auction?: Auction;
};
export default function AuctionForm({ auction }: Props) {
  const router = useRouter();
  const pathname = usePathname();

  const {
    control,
    handleSubmit,
    setFocus,
    reset,
    formState: { isSubmitting, isValid },
  } = useForm({ mode: "onTouched" });

  useEffect(() => {
    if (auction) {
      // Get the values from the object and pass them onto the reset method.
      const { make, model, color, mileage, year } = auction;
      // This method automatically pairs the field with form input (using control prop).
      reset({ make, model, color, mileage, year });
    }
    setFocus("make"); // Focus on the first field.
  }, [setFocus, auction, reset]);
  async function onSubmit(data: FieldValues) {
    try {
      let id = "";
      let response;
      if (pathname === "/auctions/create") {
        response = await createAuction(data);
        id = response.id;
      } else {
        if (auction) {
          response = await updateAuction(auction.id, data);
          id = auction.id;
        }
      }

      if (response.error) {
        throw response.error;
      } else {
        // Redirect to the created auction's details.
        router.push(`/auctions/details/${id}`);
      }
    } catch (error: any) {
      toast.error(`${error.status} ${error.message}`);
    }
  }
  return (
    <form className="flex flex-col mt-3" onSubmit={handleSubmit(onSubmit)}>
      <Input
        label={"Make"}
        name="make"
        control={control}
        rules={{ required: "Make is required" }}
      />
      <Input
        label={"Model"}
        name="model"
        control={control}
        rules={{ required: "Model is required" }}
      />
      <Input
        label={"Color"}
        name="color"
        control={control}
        rules={{ required: "Color is required" }}
      />
      <div className="grid grid-cols-2 gap-3">
        <Input
          label={"Year"}
          name="year"
          type="number"
          control={control}
          rules={{ required: "Year is required" }}
        />
        <Input
          label={"Mileage"}
          name="mileage"
          type="number"
          control={control}
          rules={{ required: "Mileage is required" }}
        />
      </div>
      {
        // Hide all this fields if we are updating an object (no auction object is passed into the form).
      }
      {!auction && (
        <>
          <Input
            label={"Image URL"}
            name="imageUrl"
            control={control}
            rules={{ required: "Image URL is required" }}
          />
          <div className="grid grid-cols-2 gap-3">
            <Input
              label={`Reserve price (enter 0 if there isn't a reserve price`}
              name="reservePrice"
              type="number"
              control={control}
              rules={{ required: "Reserve price is required" }}
            />
            <DateInput
              label={"Auction end date/time"}
              name="auctionEnd"
              control={control}
              dateFormat={"dd MMMM yyyy h:mm:a"}
              showTimeSelect
              rules={{ required: "Auction end date is required" }}
            />
          </div>
        </>
      )}
      <div className="flex justify-between">
        <Button outline color="gray">
          Cancel
        </Button>
        <Button
          type="submit"
          isProcessing={isSubmitting}
          disabled={!isValid}
          outline
          color="success"
        >
          Submit
        </Button>
      </div>
    </form>
  );
}
