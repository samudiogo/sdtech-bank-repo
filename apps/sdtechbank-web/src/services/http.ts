import axios from 'axios'

export const http = axios.create({
  baseURL: 'https://localhost:5001', // ajuste conforme sua API
}) 