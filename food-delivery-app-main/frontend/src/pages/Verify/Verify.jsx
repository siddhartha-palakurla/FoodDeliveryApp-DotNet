import React, { useContext, useEffect } from 'react';
import './Verify.css';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { StoreContext } from '../../context/StoreContext';
import axios from 'axios';

const Verify = () => {

  const [searchParams] = useSearchParams();
  const success = searchParams.get("success");
  const orderId = searchParams.get("orderId");

  const { url, token, setCartItems } = useContext(StoreContext);
  const navigate = useNavigate();

  const verifyPayment = async () => {
    try {
      const response = await axios.post(
        `${url}/api/order/verify`,
        { success, orderId },
        {
          headers: {
            Authorization: `Bearer ${token}`, // ✅ REQUIRED
            "Content-Type": "application/json"
          }
        }
      );

      if (response.data.success) {
        setCartItems({});
        navigate("/myorders");
      } else {
        navigate("/");
      }
    } catch (error) {
      console.error(
        "VERIFY ERROR:",
        error.response?.data || error.message
      );
      navigate("/");
    }
  };

  useEffect(() => {
    if (token) {
      verifyPayment();
    } else {
      // No token → user not logged in
      navigate("/login");
    }
  }, [token]);

  return (
    <div className="verify">
      <div className="spinner"></div>
    </div>
  );
};

export default Verify;
