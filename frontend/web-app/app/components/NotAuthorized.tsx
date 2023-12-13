import { Button } from 'flowbite-react'
import { title } from 'process'
import React from 'react'
import Heading from './Heading'
import { signIn } from 'next-auth/react'
type Props = {
    title: string,
    subtitle: string,
    showLogin: boolean,
    callbackUrl : string,
}
export default function NotAuthorized({title, subtitle, showLogin, callbackUrl}:Props) {
  return (
     <div className='h-[40vh] flex flex-col gap-2 justify-center items-center shadow-lg'>
        <Heading title={title} subtitle={subtitle} center />
        <div className='mt-4'>
            {showLogin && (
                <Button outline onClick={()=>signIn('id-server',{callbackUrl:'/'})}>Log in</Button>
            )}
        </div>
    </div>
  )
}
