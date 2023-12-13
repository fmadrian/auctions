"use client";

import NotAuthorized from '@/app/components/NotAuthorized'
import React from 'react'

// We get the search parameters from the query string.
export default function Page({searchParams}: {searchParams: {callbackUrl: string}}) {
  return (
    <NotAuthorized 
          showLogin
          title={`You need to log in.`}
          subtitle={`Click below to log in.`}
          callbackUrl={searchParams.callbackUrl}
    />
  )
}

// Page automatically routed.