"use client"; // 'use client' whenever the we need the client to execute some code.
import React from "react";
import Countdown, { zeroPad } from "react-countdown";

// Renderer callback with condition
const renderer = ({
  days,
  hours,
  minutes,
  seconds,
  completed,
}: {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  completed: boolean;
}) => {
  // Render a completed state
  return (
    <div
      className={`
        border-2 border-white text-white py-1 px-2 rounded-lg flex justify-center 
        ${
          // Conditional rendering
          completed
            ? "bg-red-600"
            : days === 0 && hours <= 10
            ? "bg-amber-600"
            : "bg-green-600"
        }`}
    >
      {completed ? (
        <span>Finished</span>
      ) : (
        // Hydration warning error: Generated by value rendered by the server being different
        // than the one displayed to client due to it changing while the component is rendering
        // The values in question being: days, hours, minutes, and seconds
        <span suppressHydrationWarning={true}>
          {
            // If there isn't a day left, show hours, minutes, and seconds.
            days > 0
              ? `${days} ${days === 1 ? " day" : " days"} left`
              : `${zeroPad(hours)}:${zeroPad(minutes)}:${zeroPad(seconds)}`
          }
        </span>
      )}
    </div>
  );
};

type Props = {
  auctionEnd: any;
};

export default function CountdownTimer({ auctionEnd }: Props) {
  return (
    <div>
      <Countdown date={auctionEnd} renderer={renderer} />
    </div>
  );
}
