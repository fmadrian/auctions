// We don't need 'use client' here because the parent component already uses it.
import { Button, ButtonGroup } from 'flowbite-react';
import React from 'react'
import { useParamsStore } from '../hooks/useParamsStore';
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import {BsAirplane, BsFillEmojiSmileFill, BsFillStopCircleFill, BsStopwatchFill} from 'react-icons/bs';
import {GiFinishLine, GiFlame} from 'react-icons/gi';

const pageSizeButtons = [4, 8, 12];
const orderButtons = [{
    label : 'Alphabetical',
    icon: AiOutlineSortAscending,
    value: "make"
},
{
    label : 'Recently added',
    icon: AiOutlineClockCircle,
    value: "new"
},
{
    label : 'Ending soon',
    icon: BsFillStopCircleFill,
    value: "endingSoon" // This value is not important, as it is the default sort value on the API
}]
const filterButtons = [{
    label: "Live functions",
    icon: GiFlame,
    value: "live",
                
},
{
    label: "Less than 6 hours",
    icon: GiFinishLine,
    value: "endingSoon",
                
},{
    label: "Finished",
    icon: BsStopwatchFill,
    value: "finished",
}]
export default function Filters() {
const pageSize = useParamsStore(state =>state.pageSize);
const orderBy = useParamsStore(state =>state.orderBy);
const filterBy = useParamsStore(state =>state.filterBy);
const setParams = useParamsStore(state =>state.setParams);
  return (
    <div className='flex justify-between items-center mb-4'>
            {
                // Filter by buttons.
            }
        <div>
            <span className='uppercase text-sm text-gray-500 mr-2'>Filter</span>
            <ButtonGroup>
                {filterButtons.map(({label, icon:Icon, value}) => 
                    (
                        <Button key={value}
                                onClick={ ()=> setParams({filterBy : value})}
                                color={`${filterBy === value ? "red" : "gray"}`}
                        >
                            <Icon className="mr-3 h-4 w-4"/> 
                            {label}
                        </Button>
                    )
                )}
            </ButtonGroup>
        </div>
            {
                // Order by buttons.
            }
        <div>
            <span className='uppercase text-sm text-gray-500 mr-2'>Order by</span>
            <ButtonGroup>
                {orderButtons.map(({label, icon:Icon, value}) => 
                    (
                        <Button key={value}
                                onClick={ ()=> setParams({orderBy : value})}
                                color={`${orderBy === value ? "red" : "gray"}`}
                        >
                            <Icon className="mr-3 h-4 w-4"/> 
                            {label}
                        </Button>
                    )
                )}
            </ButtonGroup>
        </div>
        {
                // Page size buttons.
        }
        <div> 
            <span className='uppercase text-sm text-gray-500 mr-2'>Page size</span>
            <ButtonGroup> 
                {pageSizeButtons.map((value, index) => 
                     <Button key={index} 
                            onClick={()=>setParams({pageSize : value})}
                            color={`${pageSize === value ? 'red' : 'gray'}`}
                            className='focus:ring-0'>
                        {value}
                    </Button>
                )}
            </ButtonGroup>
        </div>
    </div>
  )
}
