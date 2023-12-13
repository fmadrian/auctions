"use client";
import React, { useEffect, useState } from 'react'
import {FaSearch} from 'react-icons/fa';
import { useParamsStore } from '../hooks/useParamsStore';

export default function  Search() {
  // 1. Get ACTION and ACTIONS from application's state.
  const setParams = useParamsStore(state=>state.setParams);
  const searchValue = useParamsStore(state =>state.searchValue);
  const setSearchValue =useParamsStore(state =>state.setSearchValue);

  // 4. Define event to be called when the search input changes.
  function onChange(e:any){
    setSearchValue(e.target.value); // Execute the state's ACTION.
  }
  function search(){
    // Turn the state value (searchValue) into our searchTerm
    setParams({searchTerm : searchValue});
  }
  return (
    <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
        <input type='text' placeholder='Search for cars by make, model or color'
        onKeyDown={(e:any)=>{
          if(e.key === 'Enter'){
            search();
          }
        }}
        value={searchValue}
        onChange={onChange}
        className='
            flex-grow
            pl-5
            bg-transparent
            focus:outline-none
            border-transparent
            focus:border-transparent
            focus:ring-0
            text-sm
            text-gray-600
        '/>
        <button onClick={search}> <FaSearch size={34} className="bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2"/> </button>
    </div>
  )
}
