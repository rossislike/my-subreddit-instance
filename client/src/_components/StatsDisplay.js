import { useQuery } from "@tanstack/react-query"
import { fetchRedditStats } from "../services/apiService"
import StatsList from "./StatsList"
import StatsChart from "./StatsChart"
import { useState } from "react"

export default function StatsDisplay() {
  const [subreddit, setSubreddit] = useState("programming")
  const {
    isLoading,
    data: stats,
    error,
  } = useQuery({
    queryKey: ["redditStats"],
    queryFn: () => fetchRedditStats(subreddit),
    refetchInterval: 30000,
  })

  if (isLoading) {
    return (
      <div className="flex justify-center items-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-md p-4">
        <p className="text-red-800">Error loading statistics</p>
      </div>
    )
  }

  return (
    <div className="space-y-8">
      <label htmlFor="subreddit" className="text-xl font-semibold mb-4 p-2">
        Subreddit
      </label>
      <input
        type="text"
        value={subreddit}
        onChange={(e) => setSubreddit(e.target.value)}
        className="mb-4 p-2 border border-gray-300 rounded-md w-auto"
      />

      <h1 className="text-xl font-semibold mb-4  text-center">
        {`r/${subreddit}`} stats
      </h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Top Posts</h2>
          <StatsList
            items={stats.topPosts.map((post) => ({
              title: post.title,
              value: `${post.likes} upvotes`,
              subtitle: `by ${post.author}`,
            }))}
          />
        </div>
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Top Users</h2>
          <StatsList
            items={stats.topUsers.map((user) => ({
              title: user.name,
              value: `${user.posts} posts`,
            }))}
          />
        </div>
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Post Activity</h2>
          <StatsChart data={stats.topPosts} />
        </div>
      </div>
    </div>
  )
}
