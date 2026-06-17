import mongoose from "mongoose";

export const connectDB = async () =>{
    await mongoose.connect('mongodb+srv://CCT:66991882@cluster0.sxiwfc3.mongodb.net/food-del').then(()=>console.log("DB Connected"))
}