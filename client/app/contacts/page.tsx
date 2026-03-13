import { Button, Card, TextInput, Textarea } from "flowbite-react";
import {
  HiChatAlt2,
  HiLocationMarker,
  HiMail,
  HiPhone,
  HiUserGroup,
} from "react-icons/hi";

export default function ContactsPage() {
  return (
    <div className="p-6 space-y-8 bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold dark:text-white mb-2">Контакты</h1>
        <p className="text-gray-600 dark:text-gray-400 mb-8">
          Свяжитесь с нашей командой поддержки или отделом продаж
        </p>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Инфо-карточки */}
          <div className="space-y-6">
            <Card>
              <div className="flex items-center gap-4">
                <div className="p-3 bg-blue-100 dark:bg-blue-900 rounded-lg text-blue-600 dark:text-blue-300">
                  <HiPhone className="w-6 h-6" />
                </div>
                <div>
                  <p className="text-sm text-gray-500">Общий отдел</p>
                  <p className="font-bold dark:text-white">
                    +7 (777) 000-00-00
                  </p>
                </div>
              </div>
            </Card>

            <Card>
              <div className="flex items-center gap-4">
                <div className="p-3 bg-green-100 dark:bg-green-900 rounded-lg text-green-600 dark:text-green-300">
                  <HiMail className="w-6 h-6" />
                </div>
                <div>
                  <p className="text-sm text-gray-500">Email</p>
                  <p className="font-bold dark:text-white">
                    support@accounting-app.kz
                  </p>
                </div>
              </div>
            </Card>

            <Card>
              <div className="flex items-center gap-4">
                <div className="p-3 bg-purple-100 dark:bg-purple-900 rounded-lg text-purple-600 dark:text-purple-300">
                  <HiLocationMarker className="w-6 h-6" />
                </div>
                <div>
                  <p className="text-sm text-gray-500">Главный офис</p>
                  <p className="font-bold dark:text-white">
                    г. Алматы, пр. Аль-Фараби, 77
                  </p>
                </div>
              </div>
            </Card>
          </div>

          {/* Форма обратной связи */}
          <div className="lg:col-span-2">
            <Card>
              <h3 className="text-xl font-bold dark:text-white mb-4 flex items-center gap-2">
                <HiChatAlt2 className="text-blue-500" /> Напишите нам
              </h3>
              <form className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <TextInput placeholder="Ваше имя" required />
                  <TextInput type="email" placeholder="Email" required />
                </div>
                <TextInput placeholder="Тема обращения" />
                <Textarea placeholder="Ваш вопрос..." required rows={4} />
                <Button type="submit" color="blue">
                  Отправить заявку
                </Button>
              </form>
            </Card>
          </div>
        </div>

        {/* Секция: Команда (Добавила для полноты страницы) */}
        <div className="mt-12">
          <h2 className="text-2xl font-bold dark:text-white mb-6 flex items-center gap-2">
            <HiUserGroup className="text-gray-400" /> Ключевые специалисты
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {[
              {
                name: "Анна Иванова",
                role: "Старший бухгалтер",
                dept: "Налоги",
              },
              { name: "Игорь Петров", role: "Юрист", dept: "Консультации" },
              { name: "Елена Смирнова", role: "Аудитор", dept: "Проверки" },
              { name: "Максим Сидоров", role: "IT-поддержка", dept: "Доступ" },
            ].map((person, i) => (
              <div
                key={i}
                className="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 text-center"
              >
                <div className="w-16 h-16 bg-gray-200 dark:bg-gray-700 rounded-full mx-auto mb-3 flex items-center justify-center">
                  <span className="text-xl text-gray-400">
                    {person.name[0]}
                  </span>
                </div>
                <h4 className="font-bold dark:text-white">{person.name}</h4>
                <p className="text-sm text-blue-600 dark:text-blue-400">
                  {person.role}
                </p>
                <p className="text-xs text-gray-500 uppercase mt-1">
                  {person.dept}
                </p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
