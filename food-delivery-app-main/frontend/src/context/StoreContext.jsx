import { createContext, useEffect, useState } from "react";
import axios from "axios";

// eslint-disable-next-line react-refresh/only-export-components
export const StoreContext = createContext(null);

const StoreContextProvider = (props) => {

  const [cartItems, setCartItems] = useState({});
  const [token, setToken] = useState("");
  const [food_list, setFood_list] = useState([]);

  // .NET backend
  const url = "http://localhost:5081";

  // 🔹 Add to cart
  const addToCart = async (itemId) => {
    const id = String(itemId); 
    setCartItems(prev => ({
      ...prev,
      [id]: (prev[id] || 0) + 1
    }));

    if (token) {
      await axios.post(
        `${url}/api/cart/add`,
        { itemId },
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );
    }
  };

  // 🔹 Remove from cart
  const removeFromCart = async (itemId) => {
    setCartItems(prev => ({
      ...prev,
      [itemId]: prev[itemId] - 1
    }));

    if (token) {
      await axios.post(
        `${url}/api/cart/remove`,
        { itemId },
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );
    }
  };

  // 🔹 Total amount
  const getTotalCartAmount = () => {
    let totalAmount = 0;

    for (const item in cartItems) {
      if (cartItems[item] > 0) {
        const itemInfo = food_list.find(
          product => product.id === item   // ✅ FIXED
        );
        if (!itemInfo) continue;
        totalAmount += itemInfo.price * cartItems[item];
      }
    }
    return totalAmount;
  };

  // 🔹 Fetch food list
  const fetchFoodList = async () => {
    const response = await axios.get(`${url}/api/food/list`);
    setFood_list(response.data.data);
  };

  // 🔹 Load cart (GET, not POST)
  const loadCartData = async (token) => {
    const response = await axios.get(
      `${url}/api/cart/get`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    setCartItems(response.data.cartData);
  };

  useEffect(() => {
    async function loadData() {
      await fetchFoodList();

      const savedToken = localStorage.getItem("token");
      if (savedToken) {
        setToken(savedToken);
        await loadCartData(savedToken);
      }
    }
    loadData();
  }, []);

  const contextValue = {
    food_list,
    cartItems,
    setCartItems,
    addToCart,
    removeFromCart,
    getTotalCartAmount,
    url,
    token,
    setToken
  };

  return (
    <StoreContext.Provider value={contextValue}>
      {props.children}
    </StoreContext.Provider>
  );
};

export default StoreContextProvider;
