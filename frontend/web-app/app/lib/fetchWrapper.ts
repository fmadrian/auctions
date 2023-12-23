// Use this to add everything requests need a make a standard way to handle errors.

import { getTokenWorkaround } from "../actions/authActions";

const baseUrl = process.env.API_URL;
async function get(url: string) {
  const requestOptions = {
    method: "GET",
    headers: await getHeaders(),
  };
  const response = await fetch(`${baseUrl}${url}`, requestOptions);
  return await handleResponse(response);
}
async function post(url: string, body: {}) {
  const requestOptions = {
    method: "POST",
    headers: await getHeaders(),
    body: JSON.stringify(body),
  };
  const response = await fetch(`${baseUrl}${url}`, requestOptions);
  return await handleResponse(response);
}

async function put(url: string, body: {}) {
  const requestOptions = {
    method: "PUT",
    headers: await getHeaders(),
    body: JSON.stringify(body),
  };
  const response = await fetch(`${baseUrl}${url}`, requestOptions);
  return await handleResponse(response);
}
// We can't use the name 'delete'.
async function delet(url: string) {
  const requestOptions = {
    method: "DELETE",
    headers: await getHeaders(),
  };
  const response = await fetch(`${baseUrl}${url}`, requestOptions);
  return await handleResponse(response);
}
async function handleResponse(response: Response): Promise<any> {
  const text = await response.text();
  let data = "";
  try {
    data = JSON.parse(text);
  } catch (error) {
    // Couldn't parse error use text returned as error.
    data = text;
  }

  if (response.ok) {
    return data || response.statusText;
  } else {
    const error = {
      status: response.status,
      message: typeof data === "string" ? data : response.statusText, // If we have a string as an error, show it.
    };
    return { error };
  }
}

async function getHeaders() {
  const token = await getTokenWorkaround();
  const headers = { "Content-type": "application/json" } as any;
  if (token) {
    headers.Authorization = `Bearer ${token.access_token}`;
  }
  return headers;
}
// Export object with the function
export const fetchWrapper = {
  get,
  post,
  put,
  delet,
};
