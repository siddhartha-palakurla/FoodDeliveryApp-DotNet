import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import {BrowserRouter} from 'react-router-dom'
import StoreContextProvider from './context/StoreContext.jsx'
import { GoogleOAuthProvider } from '@react-oauth/google';

createRoot(document.getElementById('root')).render(
  <BrowserRouter>
    <GoogleOAuthProvider clientId="888360916766-pkeckkq8lekl837cn6glo6it4rou1m9h.apps.googleusercontent.com">
      <StoreContextProvider>
        <App/>
      </StoreContextProvider>  
    </GoogleOAuthProvider>
    
  </BrowserRouter>
   
  
)
