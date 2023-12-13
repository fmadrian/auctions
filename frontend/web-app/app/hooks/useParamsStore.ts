import { createWithEqualityFn } from "zustand/traditional";

// 1. Create a type for the attributes we are going to store in the state.
type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
    searchValue:string;
    orderBy:string;
    filterBy:string;
}
// 2. Create a type for the actions that will be supported
type Actions = {
    setParams : (params: Partial<State>) => void;// Partial<State> means a State object that night not have all the parameters.
    reset : () => void;
    setSearchValue : (value:string) => void;
}
// 3. Create the initial state for the store
const initialState : State = {
    pageNumber:1,
    pageCount:1,
    pageSize:4,
    searchTerm:"",
    searchValue: "", // Value for the input field.
    orderBy:"make",
    filterBy:"all"
};
// 4. Create state store.
// Use createWithEqualityFn instead of create to be able to use 'shallow' as equalityFn.
// https://github.com/pmndrs/zustand/discussions/1937
export const useParamsStore = createWithEqualityFn<State & Actions>()((set)=>({
    ...initialState, // Define initial state
    setParams:(newParams : Partial<State>)=>{ // Define and implement each method.
        set((state)=>{
            if(newParams.pageNumber){
                return {
                    ...state,
                    pageNumber:newParams.pageNumber
                }
            }else{
                return {
                    ...state,
                    ...newParams,
                    pageNumber: 1
                }
            }
        })

    },
    reset:()=>set(initialState), // Resets the initial state using the keyword set.
    setSearchValue(value:string){
        set({searchValue:value})
    }
}));