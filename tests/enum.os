Перем юТест;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	ВсеТесты.Добавить("ТестДолженПроверитьЧтоПростоеПеречислениеПриводитсяКСтроке");
	ВсеТесты.Добавить("ТестДолженПроверитьРаботоспособностьСравненияПростогоПеречисления");
	
	Возврат ВсеТесты;
	
КонецФункции

Процедура ТестДолженПроверитьЧтоПростоеПеречислениеПриводитсяКСтроке() Экспорт
	
	ПростоеПеречисление = ПростоеПеречисление();
	
	Попытка
		Представление = Строка(ПростоеПеречисление);
	Исключение
		Представление = "";
	КонецПопытки;

	юТест.ПроверитьИстину(НЕ ПустаяСтрока(Представление), "Проверяем, что простое перечисление верно привелось к строке.");
	
КонецПроцедуры

Процедура ТестДолженПроверитьРаботоспособностьСравненияПростогоПеречисления() Экспорт
	
	ПростоеПеречисление = ПростоеПеречисление();
	
	Результат = ПростоеПеречисление = ДопустимыйЗнак.Любой;

	юТест.ПроверитьРавенство(Истина, Результат);
	
КонецПроцедуры

Функция ПростоеПеречисление()

	ТЗ = Новый ТаблицаЗначений();
	ТЗ.Колонки.Добавить("Тест");
	Квалификатор = ТЗ.Колонки[0].ТипЗначения.КвалификаторыЧисла;
	Возврат Квалификатор.ДопустимыйЗнак;

КонецФункции