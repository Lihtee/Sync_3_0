using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBuilding.MessageBuilders
{
    public abstract class DefaultMessageBuilder
    {
        /// <summary>
        /// Размер пакета.
        /// Задавать можно в конфиге.
        /// </summary>
        private int pckageSize = x;

        /// <summary>
        /// Список упакованных измеенных объектов.
        /// </summary>
        private List<ObjectChangePackage> objectChangePackages;

        public void AppendMessage(ObjectChangePackage packaged)
        {
            objectChangePackages.Add(packaged);
        }

        /// <summary>
        /// Какая-то логика по упаковке объекта в отправляемый формат.
        /// </summary>
        /// <param name="destObject">Сам объект.</param>
        /// <param name="change">Информация об изменениях.</param>
        /// <param name="packaged">Если объект ObjectChangePackage уже есть в БД, то новый создавать не будем.</param>
        public void CreateMessage(DataObject destObject, object change, ObjectChangePackage packaged = null)
        {
            var data = ConvertToSendingFormat(destObject, change);

            if (packaged == null)
            {
                packaged = new ObjectChangePackage();

                // Как-то задать описание изменения с помощью destObject и change.
            }

            packaged.data = data;

            objectChangePackages.Add(packaged);
        }

        /// <summary>
        /// Зависимая от предметной области логика преобразования объекта.
        /// </summary>
        /// <param name="destObject">Сам объект.</param>
        /// <param name="change">Инфоормация об изменениях.</param>
        /// <returns></returns>
        public abstract object ConvertToSendingFormat(DataObject destObject, object change)
        {
            return something;
        }

        /// <summary>
        /// Выдаст очередной пакет, если он готов.
        /// </summary>
        /// <returns></returns>
        public Package PopPackage()
        {
            // Если собралось достаточно измененных объектов, то упаковываем.
            if (objectChangePackages.Count == pckageSize)
            {
                return PopPackageForced();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Выдаст пакет из всех имеющихся ObjectChangePackages.
        /// </summary>
        /// <returns></returns>
        public Package PopPackageForced()
        {
            var package = new Package
            {
                ObjectChangePackages = objectChangePackages
            };

            objectChangePackages.Clear();
            package.data = CreatePackageData(package);

            return package;
        }

        /// <summary>
        ///  Зависимая от предметной области логика формирования отправляемого пакета.
        ///  На основе набора упакованных изменений соберет пакет, готовый к отправке.
        ///  Заполнит package.data.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public abstract object CreatePackageData(Package package);
    }
}
