export default function StatsList({ items }) {
  if (!items) {
    return null
  }
  return (
    <ul className="divide-y divide-gray-200">
      {items.map((item, index) => (
        <li key={index} className="py-3">
          <div className="flex justify-between">
            <div>
              <p className="text-sm font-medium text-gray-900">{item.title}</p>
              {item.subtitle && (
                <p className="text-sm text-gray-500">{item.subtitle}</p>
              )}
            </div>
            <p className="text-sm font-semibold text-gray-900">{item.value}</p>
          </div>
        </li>
      ))}
    </ul>
  )
}
