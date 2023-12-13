export {default} from "next-auth/middleware";
export const config = {
    matcher: [
        // Try to enter here...
        "/session" 
    ], pages:{
        // ... will redirect to one of these pages.
        signin: 'api/auth/signin'
    }
}