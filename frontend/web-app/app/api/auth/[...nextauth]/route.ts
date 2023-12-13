import NextAuth, { NextAuthOptions } from "next-auth"
import DuendeIdentityServer6 from "next-auth/providers/duende-identity-server6";

export const authOptions: NextAuthOptions = {
    session:{
        strategy : "jwt",    // JWT is default, but I want to see the strategy.
                            // Saves user session in an encrypted JWT.
                            // Encrypted JWT is stored in a session (HTTPOnly) cookie.
        },
        providers:[
            DuendeIdentityServer6({
                id: 'id-server', // Identifier for provider inside the application.
                clientId: 'auctionsNextApp', // Has to match the ClientId put inside the IdentityServer configuration.
                clientSecret : 'secret', // Has to match the Secret put inside the IdentityServer configuration.
                issuer: 'http://localhost:5000', // Where the provider is, so it can redirect us.
                authorization: {params:{
                    scope: 'openid profile auctionApp' // Scopes the client will use.
                }},
                idToken : true // Extract user information from the claims in the token (id_token claim)
            })
        ],
        callbacks:{
            async jwt({token,profile,account,user}){ // From the JWT return the following information: token, profile, account, and user.
                                                     // We only get this information ONCE, when we log in
                
                if(profile){
                    token.username = profile.username;
                }if(account){
                    token.access_token = account.access_token;
                }
                return token; 
            },
            async session({session, token}){
                if(token){
                    session.user.username = token.username
                }
                return session;
            }
        }                        
    
}
const handler = NextAuth(authOptions);
export { handler as GET, handler as POST }