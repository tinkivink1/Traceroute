UDP/ICMP C# traceroute (tracert)
# Важно!!!
Если не видно приходящего icmp трафика, то необходимо добавить правило в брандмауэр(фаервол), а также могут потребоваться права администратора.  Правило фаервола для винды:
<code>netsh advfirewall firewall add rule name="All ICMP v4" dir=in action=allow protocol=icmpv4:any,any</code>
# Important!!!
If you can not see the incoming icmp traffic, then you need to add a rule to the firewall, and administrator rights may also be required. Firewall rule for Windows:
<code>netsh advfirewall firewall add rule name="All ICMP v4" dir=in action=allow protocol=icmpv4:any,any</code>
