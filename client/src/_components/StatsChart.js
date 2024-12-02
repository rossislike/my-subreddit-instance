import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts"

export default function StatsChart({ data }) {
  if (!data) return null
  const chartData = data.map((post) => ({
    name: post.title.substring(0, 10) + "...",
    upvotes: post.likes,
  }))

  return (
    <div className="h-96">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData}>
          <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
          <YAxis />
          <Tooltip />
          <Bar dataKey="upvotes" fill="#4f46e5" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
