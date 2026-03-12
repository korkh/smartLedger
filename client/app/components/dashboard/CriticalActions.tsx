"use client";
import Link from "next/link";
import { HiCash, HiChevronRight, HiClock, HiExclamation } from "react-icons/hi";

const ICON_MAP = {
  ecp: HiClock,
  debt: HiCash,
  risk: HiExclamation,
};

interface CriticalAction {
  id: string;
  type: "danger" | "warning" | "info";
  category: "ecp" | "debt" | "risk";
  task: string;
  client: string;
  link: string;
}

interface Props {
  actions: CriticalAction[];
}

export default function CriticalActions({ actions }: Props) {
  // Обработка пустого состояния
  if (!actions || actions.length === 0) {
    return (
      <div className="p-8 text-center">
        <p className="text-gray-500 dark:text-gray-400 text-sm">
          Критических задач на сегодня нет. Хорошая работа!
        </p>
      </div>
    );
  }

  return (
    <div className="overflow-hidden">
      <ul className="divide-y divide-gray-200 dark:divide-gray-700">
        {actions.map((item) => {
          const Icon = ICON_MAP[item.category] || HiExclamation;

          return (
            <li
              key={item.id}
              className="p-4 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-all duration-200"
            >
              <div className="flex items-center space-x-4">
                {/* Иконка категории */}
                <div
                  className={`shrink-0 p-2 rounded-xl ${
                    item.type === "danger"
                      ? "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400"
                      : item.type === "warning"
                        ? "bg-yellow-100 text-yellow-600 dark:bg-yellow-900/30 dark:text-yellow-400"
                        : "bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400"
                  }`}
                >
                  <Icon className="w-6 h-6" />
                </div>

                {/* Текстовая информация */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center space-x-2">
                    <p className="text-sm font-bold text-gray-900 dark:text-white truncate">
                      {item.task}
                    </p>
                    {item.type === "danger" && (
                      <span className="shrink-0 px-1.5 py-0.5 text-[10px] uppercase font-black bg-red-100 text-red-700 rounded dark:bg-red-900 dark:text-red-300">
                        Срочно
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-400 truncate mt-0.5">
                    {item.client}
                  </p>
                </div>

                <Link
                  href={item.link}
                  className="flex items-center space-x-1 text-xs font-bold text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 transition-colors"
                >
                  <span>Открыть</span>
                  <HiChevronRight className="w-4 h-4" />
                </Link>
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
