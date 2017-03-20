(require '[clojure.math.combinatorics :as combo])

(defn rand-int32 []
  (* (rand-int (bit-shift-left 1 31))
     (if (>= (rand) 0.5) 1 -1)))

(defn rand-distinct-reg [n]
  (-> (reduce (fn [m _]
                (loop [r (rand-int 32)]
                  (if (contains? m r)
                    (recur (rand-int 32))
                    (conj m r))))
              #{} (range n))
      vec))
(defn rand-uint32 []
  (* (long (rand-int (bit-shift-left 1 31)))
     (if (>= (rand) 0.5) 2 1)))

; Constants
(def MAX-INT32 (dec (bit-shift-left 1 31)))
(def MIN-INT32 (- (bit-shift-left 1 31)))
(def MAX-UINT32 (dec (bit-shift-left 1 32)))
(def MIN-UINT32 0)

(defn to-uint32
  "I am quite sure this is a hack ... reminding me why high level
  languages are not cool for bit programming" 
  [x]
  (bit-and (long x) 0x00000000FFFFFFFF))

(defn rand-reg []
  (inc (rand-int 25)))

(defn format-uint32 [x]
  (cond (string? x)
        x
        (number? x)
        (to-uint32 x)))

(defn gen-arithmetic-with-zeroes
  "n is the number of zero registers. n can be a value from 0 to 2 inclusive" 
  [n {:keys [prng fnc edge-cases formatter] :or {formatter identity}}]
  (with-out-str
    (doseq [indices (combo/combinations [0 1 2] n)]
      (let [zero-reg?
              (fn [i]
                (some (partial = i) (vec indices)))
            our-rand-reg
              (fn [i]
                (if (zero-reg? i) 0 (rand-reg)))
            a (prng)
            b (prng)
            rs (our-rand-reg 0)
            rt (our-rand-reg 1)
            rd (our-rand-reg 2)
            res (cond (= rd 0)
                      0
                      ; a would have irrelevant by now
                      (= rs rt)
                      (fnc (if (zero-reg? 1) 0 b)
                           (if (zero-reg? 1) 0 b))
                      ; run fnc with the values in register
                      ; need to check if the register is 0 or 1
                      :else
                      (fnc (if (zero-reg? 0) 0 a)
                           (if (zero-reg? 1) 0 b)))]
        (println rs (formatter a)
                 rt (formatter b)
                 rd (formatter res))))))

(defn gen-arithmetic-edge-cases
  [edge-cases {:keys [prng fnc formatter] :or {formatter identity}}]
  (with-out-str
    (doseq [[x y] (combo/combinations (conj edge-cases (prng) (prng)) 2)]
      (println 1 (formatter x)
               2 (formatter y)
               3 (formatter (fnc x y)))
      (println 1 (formatter x)
               0 (formatter y)
               3 (formatter (fnc x 0))))))

(defn gen-multdiv-with-zeroes
  "n is the number of zero registers. n can be a value from 0 to 2 inclusive" 
  [n {:keys [prng fnc-lo fnc-hi formatter] :or {formatter identity}}]
  (with-out-str
    (doseq [indices (combo/combinations [0 1 2 3] n)]
      (let [zero-reg?
              (fn [i]
                (some (partial = i) (vec indices)))
            our-rand-reg
              (fn [i]
                (if (zero-reg? i) 0 (rand-reg)))
            rs-value (prng)
            rt-value (prng)
            [rs rt r-lo r-hi] (rand-distinct-reg 4)
            rs   (if (zero-reg? 0) 0 rs)
            rt   (if (zero-reg? 1) 0 rt)
            r-lo (if (zero-reg? 2) 0 r-lo)
            r-hi (if (zero-reg? 3) 0 r-hi)
            get-rs #(if (= 0 rs) 0 rs-value)
            get-rt #(if (= 0 rt) 0 rt-value)]

        (println rs (formatter rs-value)
                 rt (formatter rt-value)
                 r-lo (if (= r-lo 0)
                        0 (formatter (fnc-lo (get-rs) (get-rt))))
                 r-hi (if (= r-hi 0)
                        0 (formatter (fnc-hi (get-rs) (get-rt)))))))))

(defn gen-multdiv-data [{:keys [prng fnc-lo fnc-hi edge-cases] :as args}]
  (assert (and prng fnc-lo fnc-hi))
  (with-out-str
    (dotimes [i 5]
      (dotimes [j 10]
        (print (gen-multdiv-with-zeroes i args))))))

(defn signed-add [a b]
  (let [res (+ a b)]
    (if (or (and (< a 0) (< b 0)
                 (< res -0x80000000))
            (and (> a 0) (> b 0)
                 (>= res 0x80000000)))
        "OVERFLOW"
        res)))

(defn abs [a]
  (if (> a 0) a (- a)))

(defn signed-sub [a b]
  (signed-add a (- b)))

(defn checked-div-hi [a b]
  (if (= b 0)
    "UNDEFINED"
    ; force the remainder to a positive value
    (let [remainder (mod (+ (abs b) (mod a b)) (abs b))]
      (if (< a 0)
        (- remainder (abs b))
        remainder))))

(defn checked-div-lo [a b]
  (if (= b 0)
    "UNDEFINED"
    (int (/ a b))))

(defn gen-arithmetic-data [{:keys [prng fnc edge-cases] :as args}]
  (assert (and prng fnc))
  (with-out-str
    (dotimes [i 4]
      (dotimes [j 5]
        (print (gen-arithmetic-with-zeroes
                 i args))))
    (print (gen-arithmetic-edge-cases
             edge-cases args))))


["AND"  bit-and]
["OR"   bit-or]
["XOR"  bit-xor]
["ADDU" +]
["SUBU" -]
["SLTU" (fn [a b] (if (< a b) 1 0))]
["ADD" signed-add]
["SUB" signed-sub]
["SLT" (fn [a b] (if (< a b) 1 0))]
["SLLV" (fn [a b] (bit-shift-left b (bit-and a 0x1F)))]
["SRAV" (fn [a b] (bit-shift-right b (bit-and a 0x1F)))]
["SRLV" (fn [a b] (bit-shift-right b (bit-and a 0x1F)))]
["SRL" (fn [a b] (bit-shift-right a (bit-and b 0x1F)))]
["SRA" (fn [a b] (bit-shift-right a (bit-and b 0x1F)))]


(defn run []
  (spit "DIV.txt"
        (gen-multdiv-data
          {:prng rand-int32
           :fnc-hi checked-div-hi
           :fnc-lo checked-div-lo
           :formatter identity})))

(comment
  ; Multiplication (change the prng for signed / unsigned)
  (spit "MULT.txt"
        (gen-multdiv-data
          {:prng rand-int32
           :fnc-hi #(bit-and
                      (bit-shift-right (* (long %1) (long %2)) 32)
                      0xFFFFFFFF)
           :fnc-lo #(bit-and 0xFFFFFFFF
                             (* (long %1) (long %2)))
           :formatter identity}))
  ; unsigned stuff
  (doseq [[instruction fnc] []]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-uint32 
                                  :fnc fnc
                                  :formatter to-uint32
                                  :edge-cases [MIN-UINT32 MAX-UINT32 0 1]})))
  ; Signed stuff
  (doseq [[instruction fnc] []]
    (spit (str instruction ".txt")
            (gen-arithmetic-data {:prng rand-int32
                                  :fnc fnc
                                  :edge-cases [MIN-INT32 MIN-INT32 0 -1 1]}))))
  
