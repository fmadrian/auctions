"use client" // Client component because it uses client functionality (react hooks)
import React, { useEffect, useState } from "react";
import AuctionCard from "./AuctionCard";
import AppPagination from "../components/AppPagination";
import { getData } from "../actions/auctionActions";
import { Auction, PagedResult } from "../types/AppTypes";
import Filters from "./Filters";
import { useParamsStore } from "../hooks/useParamsStore";
import { shallow } from "zustand/shallow";
import qs from "query-string";
import EmptyFilter from "../components/EmptyFilter";

export default function Listings() {
  // 1. Set the state for the component.
  const [isLoading, setIsLoading] = useState(false);
  const [data, setData]= useState<PagedResult<Auction>>();
  // 2. Get parameters(STATE) and ACTIONS (application's state) from the store.
  // shallow ensures we don't get all of the state back.
  // All these attributes in params get put in the query.
  const params = useParamsStore(
    (state)=>({
      pageNumber: state.pageNumber,
      pageSize: state.pageSize,
      searchTerm:state.searchTerm,
      orderBy:state.orderBy,
      filterBy:state.filterBy
    }), shallow);
    // 3. Get ACTION from application's state.
  const setParams = useParamsStore(state=>state.setParams);
  // 4. Create the url using the query-string library
  const url = qs.stringifyUrl({url: '',query:params})
  // 5. Create function to change page number
  function setPageNumber(pageNumber:number){
    setParams({pageNumber}); // Goes to the store and executes this action (setParams).
  }

  // useEffect allows us to do a side effect after the component first loads.
  useEffect(
    // The code we put here executes the first time the component loads.
    // Depending on the code here the component might re-render.
    ()=>{
      // 2. Retrieve the data using the standard promises.
      setIsLoading(true);
      getData(url).then(
        (data)=>{
          setData(data)
        }
      ).finally(()=>{
        setIsLoading(false);
      });
    }, [url]); // If this array was empty, it means that THIS CODE RUNS ONCE (because we are NOT BINDING ANY VARIABLES TO IT).
                    // But we are binding pageSize, so EACH TIME ANY OF THE BINDED VARIABLES CHANGES, 
                    // THE CODE RUNS AGAIN AND THE COMPONENT GETS RE-RENDERED. 
     
  // What to return when the component is loading.
  if(isLoading){
    return (
      <h3>Loading...</h3>
    )
  }
  // What to return after the component loads and has data.
  return (
    <>
    {
      // We no longer need to pass parameters to filter component, 
      // we can get them from the store.
    }
    <Filters/>
    {data && data.totalCount == 0 ?  
      <EmptyFilter showReset/>
      : (
          <>
            <div className="grid grid-cols-4 gap-6">
              {data && data.results.length > 0 && data.results.map((auction) => {
              
                // 1. Pass the key and the props for each auction.
                return <AuctionCard key={auction.id} auction={auction} />;
              })}
            </div>
            <div className="flex justify-center mt-4">
              <AppPagination currentPage={params.pageNumber} pageCount={data ? data.pageCount : 1} pageChanged={setPageNumber}/>
            </div>
          </>
        )
    }
    </>
  );
}
