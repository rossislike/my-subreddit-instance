import axios from "axios"

// const API_BASE_URL = "http://localhost:5242/api"

export const fetchData = async () => {
  try {
    // const response = await axios.get(`${API_BASE_URL}/test`)
    const response = await axios.get(`${process.env.REACT_APP_API_URL}/test`)
    return response.data
  } catch (err) {
    console.error(err)
    throw err
  }
}

export const fetchRedditAuth = async () => {
  try {
    // const response = await axios.get(`${API_BASE_URL}/test`)
    const response = await axios.get(`${process.env.REACT_APP_BASE_URL}`)
    return response.data
  } catch (err) {
    console.error(err)
    throw err
  }
}

export const fetchRedditStats = async (subreddit) => {
  try {
    // const response = await axios.get(`${API_BASE_URL}/test`)
    const response = await axios.get(
      `${process.env.REACT_APP_API_URL}/stats?subreddit=${subreddit}`
    )
    return response.data
  } catch (err) {
    console.error(err)
    throw err
  }
}
