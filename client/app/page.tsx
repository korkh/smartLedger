"use client";
import { Button } from "flowbite-react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import {
  HiChartBar,
  HiChip,
  HiLightningBolt,
  HiShieldCheck,
  HiUserGroup,
} from "react-icons/hi";

export default function Home() {
  const { data: session } = useSession();

  return (
    <main className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Hero Section */}
      <section className="bg-white dark:bg-gray-900">
        <div className="py-8 px-4 mx-auto max-w-7xl text-center lg:py-16">
          <h1 className="mb-4 text-4xl font-extrabold tracking-tight leading-none text-gray-900 md:text-5xl lg:text-6xl dark:text-white">
            Управляйте бухгалтерией с умом
          </h1>
          <p className="mb-8 text-lg font-normal text-gray-500 lg:text-xl sm:px-16 lg:px-48 dark:text-gray-400">
            Smart Ledger объединяет автоматизацию отчетности и анализ данных с
            помощью ИИ для вашего бизнеса.
          </p>
          <div className="flex flex-col space-y-4 sm:flex-row sm:justify-center sm:space-y-0 gap-3">
            <Button
              size="xl"
              as={Link}
              href={session ? "/dashboard" : "/login"}
            >
              {session ? "Перейти в Дашборд" : "Начать работу"}
            </Button>

            {/* Кнопка теперь ведет к якорю #features */}
            <Button
              size="xl"
              color="light"
              onClick={() =>
                document
                  .getElementById("features")
                  ?.scrollIntoView({ behavior: "smooth" })
              }
            >
              Узнать больше
            </Button>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-16 bg-gray-50 dark:bg-gray-800">
        <div className="px-4 mx-auto max-w-7xl">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold dark:text-white">
              Почему выбирают Smart Ledger?
            </h2>
            <p className="text-gray-500 dark:text-gray-400 mt-4">
              Мы автоматизируем рутину, чтобы вы могли сосредоточиться на росте.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            <FeatureCard
              icon={HiLightningBolt}
              title="Скорость"
              desc="Автоматическая обработка транзакций и мгновенная генерация отчетов."
            />
            <FeatureCard
              icon={HiChip}
              title="ИИ-аналитика"
              desc="Прогнозирование налоговых рисков и анализ рентабельности с помощью ИИ."
            />
            <FeatureCard
              icon={HiShieldCheck}
              title="Безопасность"
              desc="Шифрование данных банковского уровня и полное соответствие стандартам."
            />
            <FeatureCard
              icon={HiChartBar}
              title="Прозрачность"
              desc="Наглядные графики выручки, долгов и операционных показателей."
            />
          </div>
        </div>
      </section>
      {/* Детальные секции */}
      <section className="py-16 space-y-24">
        {/* Блок 1: ИИ Аналитика и Тенге */}
        <div className="px-4 mx-auto max-w-7xl">
          <div className="flex flex-col lg:flex-row items-center gap-12">
            <div className="lg:w-1/2 space-y-6">
              <span className="text-blue-600 font-semibold tracking-wider uppercase text-sm">
                Интеллект системы
              </span>
              <h2 className="text-3xl md:text-4xl font-bold dark:text-white">
                Умный контроль финансов в ₸ и сом
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-400">
                Smart Ledger адаптирован под специфику учета в Центральной Азии.
                Система автоматически конвертирует валютные операции и
                анализирует рентабельность вашего бизнеса с учетом локальных
                налоговых ставок.
              </p>
              <ul className="space-y-3">
                {[
                  "Мониторинг порога по НДС в режиме реального времени",
                  "Авто-расчет налогов для ИП и ТОО",
                  "Интеграция с локальными API",
                ].map((item, i) => (
                  <li
                    key={i}
                    className="flex items-center gap-3 dark:text-gray-300"
                  >
                    <div className="shrink-0 w-5 h-5 bg-blue-100 dark:bg-blue-900 rounded-full flex items-center justify-center">
                      <div className="w-2 h-2 bg-blue-500 rounded-full" />
                    </div>
                    {item}
                  </li>
                ))}
              </ul>
            </div>
            <div className="lg:w-1/2 bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-2xl border border-gray-100 dark:border-gray-700">
              {/* Здесь можно сымитировать график прибыли */}
              <div className="space-y-4">
                <div className="h-4 bg-gray-100 dark:bg-gray-700 rounded w-3/4"></div>
                <div className="h-32 bg-blue-50 dark:bg-blue-900/30 rounded-xl flex items-end justify-around p-4 gap-2">
                  {[40, 70, 45, 90, 65, 80].map((h, i) => (
                    <div
                      key={i}
                      className="bg-blue-500 w-full rounded-t"
                      style={{ height: `${h}%` }}
                    ></div>
                  ))}
                </div>
                <p className="text-xs text-center text-gray-400">
                  Прогноз выручки на основе данных ИИ
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Блок 2: Трансграничный бизнес (КЗ + КГ) */}
        <div className="bg-white dark:bg-gray-800 py-16">
          <div className="px-4 mx-auto max-w-7xl">
            <div className="flex flex-col lg:flex-row-reverse items-center gap-12">
              <div className="lg:w-1/2 space-y-6">
                <span className="text-purple-600 font-semibold tracking-wider uppercase text-sm">
                  ЕАЭС Совместимость
                </span>
                <h2 className="text-3xl md:text-4xl font-bold dark:text-white">
                  Работайте по всему СНГ без границ
                </h2>
                <p className="text-lg text-gray-600 dark:text-gray-400">
                  Если ваш бизнес ведет деятельность в Казахстане и Кыргызстане
                  одновременно, Smart Ledger поможет консолидировать отчетность.
                  Мы учитываем особенности учета импорта/экспорта внутри ЕАЭС.
                </p>
                <div className="grid grid-cols-2 gap-4 pt-4">
                  <div className="p-4 border border-gray-100 dark:border-gray-700 rounded-xl shadow-sm">
                    <h4 className="font-bold dark:text-white">Казахстан</h4>
                    <p className="text-xs text-gray-500">
                      СНТ, Виртуальный склад, ЭСФ
                    </p>
                  </div>
                  <div className="p-4 border border-gray-100 dark:border-gray-700 rounded-xl shadow-sm">
                    <h4 className="font-bold dark:text-white">Кыргызстан</h4>
                    <p className="text-xs text-gray-500">
                      ЭТТН, ЭСФ, отчеты ГНС
                    </p>
                  </div>
                </div>
              </div>
              <div className="lg:w-1/2 flex justify-center">
                <div className="relative w-64 h-64 bg-purple-100 dark:bg-purple-900/20 rounded-full flex items-center justify-center">
                  <HiUserGroup className="w-32 h-32 text-purple-500/40" />
                  <div className="absolute top-0 right-0 p-3 bg-white dark:bg-gray-700 rounded-lg shadow-lg text-sm font-bold">
                    🇰🇿 KZ
                  </div>
                  <div className="absolute bottom-0 left-0 p-3 bg-white dark:bg-gray-700 rounded-lg shadow-lg text-sm font-bold">
                    🇰🇬 KG
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Блок 3: Безопасность и Доверие */}
        <div className="px-4 mx-auto max-w-7xl pb-16">
          <div className="flex flex-col lg:flex-row items-center gap-12">
            <div className="lg:w-1/2 space-y-6">
              <span className="text-green-600 font-semibold tracking-wider uppercase text-sm">
                Надежность
              </span>
              <h2 className="text-3xl md:text-4xl font-bold dark:text-white">
                Облачное решение с локальной поддержкой
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-400">
                Ваши данные хранятся в защищенных дата-центрах, а наша команда
                поддержки говорит на вашем языке и знает тонкости локального
                бухучета.
              </p>
              <div className="flex gap-4">
                <Button color="blue" size="lg">
                  Попробовать демо
                </Button>
                <Button color="light" size="lg">
                  Скачать презентацию
                </Button>
              </div>
            </div>
            <div className="lg:w-1/2">
              <div className="grid grid-cols-1 gap-4">
                <div className="p-6 bg-green-50 dark:bg-green-900/10 rounded-2xl border border-green-100 dark:border-green-800/30">
                  <h4 className="font-bold text-green-700 dark:text-green-400 mb-1">
                    99.9% Uptime
                  </h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Доступ к бухгалтерии 24/7 из любой точки мира.
                  </p>
                </div>
                <div className="p-6 bg-blue-50 dark:bg-blue-900/10 rounded-2xl border border-blue-100 dark:border-blue-800/30">
                  <h4 className="font-bold text-blue-700 dark:text-blue-400 mb-1">
                    SSL Protection
                  </h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Все данные шифруются по протоколам банковского стандарта.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}

// Вспомогательный компонент для карточек
function FeatureCard({
  icon: Icon,
  title,
  desc,
}: {
  icon: any;
  title: string;
  desc: string;
}) {
  return (
    <div className="p-6 bg-white dark:bg-gray-700 rounded-xl shadow-sm border border-gray-100 dark:border-gray-600">
      <div className="w-12 h-12 bg-blue-100 dark:bg-blue-900 rounded-lg flex items-center justify-center mb-4 text-blue-600 dark:text-blue-300">
        <Icon className="w-6 h-6" />
      </div>
      <h3 className="text-xl font-bold mb-2 dark:text-white">{title}</h3>
      <p className="text-gray-500 dark:text-gray-400 text-sm leading-relaxed">
        {desc}
      </p>
    </div>
  );
}
