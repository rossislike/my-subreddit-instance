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
  return (
    <QueryClientProvider client={queryClient}>
      <ReactQueryDevtools initialIsOpen={false} />
      <main className="container mx-auto px-4 py-8">
        <RedditAuth />
        <StatsDisplay />
      </main>
    </QueryClientProvider>
  )
}

export default App
