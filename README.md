# Co to za projekt?
Program ma w tej chwili dwa główne przeznaczenia :
* Kodowanie obrazka Watermark do obrazu oryginalnego
* Wyciągnięcie obrazka Watermark z obrazka zakodowanego wcześniej przez program

Poniższy opis będzie więc podzielony na opis dwóch tych funkcjonalności

# Opis aplikacji
Po uruchomieniu aplikacji użytkownik powinien wykonać jedną z dwóch ścieżek. Definiuje się je za pomocą wybrania jednego z radio buttonów `Encode` lub `Decode`

## Wybor algorytmu
Użykownik ma do wyboru dwa algorytmy do kodowania i dekodowania obrazków: 
* DCT - transformacja cosinusowa
* FourierAlgorihm - transformata Fouriera 

## Encode
Funkcja kodowania watermarka wymusza na użytkowniku następującej ścieżki kroków do wykonania:
1. Wciśnij opcje `Encode`
1. Wciśnij przycisk `Choose image`
1. W wyświetlonym dialogu podaj ścieżkę do obrazka, w którym zakodowany ma być Watermakr. Po wybraniu, obrazek po chwili powinien zostać wczytany do aplikacji i być widoczny po lewej stronie
1. Wciśnij przycisk `Choose watermark`
1. W wyświetlonym dialogu podaj ścieżkę do obrazka, który ma zostać zakodowany w pierwszym, wybranym obrazku. Wybrany orazek po chwili powinien pojawić się na ekranie.
1. Wciśnij przycisk `Encode`
1. Program w tym momencie koduje watermark do obrazka oryginalnego
1. Po zakończonym procesie kodowania, program pokaże stosowny komunikat.
1. Zapisz zakodowany obrazek przyciskiem `Save` wskazując ścieżkę do zapisania pliku
1. Program zresetuje interfejs

## Decode
Funckja dekodowania pozwala wybrac obrazek oryginalny oraz wybrac okrazek z watermarkiem i wyciągnąć na podstawie różnicy sam watermark. Wykonaj następująco:

1. Wciśnij opcję `Decode`
1. Wciśnij przycisk `Choose image`
1. Podaj ścieżkę do obrazka oryginalnego, po chwili pojawi się on w aplikacji 
1. Wciśnij przycisk `Choose watermarked image`
1. Podaj ścieżkę do obrazka zakodowanego, po chwili pojawi się on w aplikacji, obok oryginału
1. Wciśnij przycisk `Decode`
1. Program w tym momencie wyciąga na podstawie obu obrazków watermark z obrazka zakodowanego.
1. Po zakończonym procesie odkodowywania, program pokaże stosowny komunikat


# Limity
* Obrazki muszą mięc następujące formaty 
    * jpg
    * jpeg
    * png
    * bmp
* Obrazek **watermark** musi być rozmiaru **64x64 pikseli!** 
* DCT
    * Wyciągany watermark jest różnym natężeniem koloru niebieskiego
* Fourier
    * Obrazki kodowane są w skali szarości
 

# Przyszłe prace
Następnymi krokami w następującym oprogramowaniu będzie:
 
1. Próba odczytania informacji o możliwym zakodowaniu obrazka tylko na podstawie obrazka już zakodowanego - bez oryginału.
    * Prawdopodobieństwo zakodowania ?
2. Próba określenia czy obrazek został zakodowany, a jeżeli tak to przez jaki algorytm? 
3. Pokazanie różnic pomiędzy algorytmami? 
4. Parametryzacja algorytmów ?
5. Różne wzory podmiany kodowanej wartości?
