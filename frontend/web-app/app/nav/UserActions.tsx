"use client";
import { Dropdown } from 'flowbite-react'
import { User } from 'next-auth';
import { signOut } from 'next-auth/react';
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation';
import React from 'react'
import { AiFillCar, AiFillTrophy } from 'react-icons/ai';
import { HiCog, HiUser } from 'react-icons/hi';
import { useParamsStore } from '../hooks/useParamsStore';


type Props = {
  user: User
}
export default function UserActions({user} : Props) {
  const router = useRouter();
  const pathname = usePathname();
  const setParams = useParamsStore(state => state.setParams)

  function setWinner(){
    setParams({winner: user.username, seller:undefined})
    if(pathname !== '/') router.push("/") // Redirect to home if it isn't in home.
  }
  function setSeller(){
    setParams({seller: user.username, winner:undefined})
    if(pathname !== '/') router.push("/") // Redirect to home if it isn't in home.
  }
  return (
    <Dropdown
    inline label={`Welcome ${user.name}`}>
      
      <Dropdown.Item icon={HiUser} onClick={setSeller}>
        <Link href="/">
          My auctions
        </Link>
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillTrophy} onClick={setWinner}>
        <Link href="/">
          Auctions won
        </Link>
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillCar}>
        <Link href="/auctions/create">
          Sell my car
        </Link>
      </Dropdown.Item>
      <Dropdown.Item icon={HiCog}>
        <Link href="/session">
          Session (dev only)
        </Link>
      </Dropdown.Item>
      <Dropdown.Divider/>
      <Dropdown.Item onClick={()=> signOut({callbackUrl : "/"})}>
        <Link href="/">
          Sign out
        </Link>
      </Dropdown.Item> 
      </Dropdown > 
    )
}
