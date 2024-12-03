import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { ReactQueryDevtools } from "@tanstack/react-query-devtools"
import RedditAuth from "./_components/RedditAuth"
import StatsDisplay from "./_components/StatsDisplay"

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
    },
  },
})

function App() {
  const queryParams = new URLSearchParams(window.location.search)
  const username = queryParams.get("username")

  console.log("REACT_APP_API_URL", process.env.REACT_APP_API_URL)
  return (
    <QueryClientProvider client={queryClient}>
      <ReactQueryDevtools initialIsOpen={false} />
      <main className="container mx-auto px-4 py-8">
        <RedditAuth />
        {username && (
          <>
            <h2>
              <span className="text-black">Welcome {username}</span>
            </h2>
            <StatsDisplay />
          </>
        )}
      </main>
    </QueryClientProvider>
  )
}

export default App
