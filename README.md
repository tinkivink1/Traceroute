# Описание
Данный c# скрипт является моей реализацией уже существующей утилиты traceroute. Преимущество моей реализации заключается в том, что запуск скрипта возможен как с использование ICMP протокола, так и UDP, 
так как на windows traceroute работает только с icmp, а на linux по udp (правда потом выяснилось, что есть еще и tracert который работает с ICMP протоколом, но кто вообще будет читать этот ридми)
  
### Инструкция к запуску: 
1. Открыть терминал и перейти в файл бинарника
2. Выполнить команду Traceroute [PROTOCOL (icmp | udp)] [REMOTE IP:REMOTE PORT (127.0.0.1:1234)] [SOURCE PORT (udp only)] [SIZE] [MAX TTL] [ATTEMPTS] [TIMEOUT]

### Важно!!!
Если не видно приходящего icmp трафика, то необходимо добавить правило в брандмауэр(фаервол), а также могут потребоваться права администратора.  Правило фаервола для винды:
<code>netsh advfirewall firewall add rule name="All ICMP v4" dir=in action=allow protocol=icmpv4:any,any</code>  

### Important!!!
If you can not see the incoming icmp traffic, then you need to add a rule to the firewall, and administrator rights may also be required. Firewall rule for Windows:
<code>netsh advfirewall firewall add rule name="All ICMP v4" dir=in action=allow protocol=icmpv4:any,any</code>

# Демонстрация
![image](https://github.com/tinkivink1/Traceroute/assets/92641773/e5ba45be-03af-4d79-88f0-35795568571e)
