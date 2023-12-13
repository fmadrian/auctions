"use client";

import React from 'react'
import { AiOutlineCar } from 'react-icons/ai'
import { useParamsStore } from '../hooks/useParamsStore';
import { usePathname, useRouter } from 'next/navigation';

export default function Logo() {
  // 1. Retrieve reset ACTION from state.
    const reset = useParamsStore(state=>state.reset);
  const router = useRouter();
  const pathname = usePathname();
  
  function doReset(){
    if(pathname === "/"){
      reset();
    }else{
      router.push("/"); 
    }
  }

  
    return (
      <div onClick={doReset} className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-green-800">
        <AiOutlineCar size={34} />
        <div>Auctions</div>
    </div>
  )
}
