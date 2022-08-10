# Важно!!!
Если не видно приходящего icmp трафика, то необходимо добавить правило в брандмауэр(фаервол). Для винды:\n
<code>netsh advfirewall firewall add rule name="All ICMP v4" dir=in action=allow protocol=icmpv4:any,any</code>
# Important!!!
If you can not see the incoming icmp traffic, then you need to add a rule to the (firewall). For Windows:\n
<code>netsh advfirewall firewall add rule name="All ICMP v4" dir=in action=allow protocol=icmpv4:any,any</code>
