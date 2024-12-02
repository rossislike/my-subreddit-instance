import { fetchRedditAuth } from "../services/apiService"
import { useQuery } from "@tanstack/react-query"

export default function RedditAuth() {
  const { isLoading, data: auth } = useQuery({
    queryKey: ["redditAuth"],
    queryFn: fetchRedditAuth,
  })

  if (isLoading) return "Loading..."
  return (
    <div>
      {auth && (
        <a
          className="text-blue-500 hover:text-blue-700 hover:underline"
          href={decodeURI(auth.authUrl)}
        >
          Reddit Sign in
        </a>
      )}
    </div>
  )
}
